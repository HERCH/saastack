using Application.Persistence.Interfaces;
using Common;
using Common.Extensions;
using Infrastructure.Persistence.Interfaces;
using JetBrains.Annotations;
using MimeKit;

namespace Infrastructure.Persistence.OnPremises.ApplicationServices;

[UsedImplicitly]
public sealed class FileSystemBlobStore : IBlobStore, IDisposable
{
    private readonly string _rootPath;
    private readonly IRecorder _recorder;
    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private readonly Dictionary<string, bool> _verifiedContainers = new();

    public static FileSystemBlobStore Create(IRecorder recorder, FileSystemBlobStoreOptions options)
    {
        Directory.CreateDirectory(options.RootPath); // Asegura que el directorio raíz exista
        return new FileSystemBlobStore(recorder, options.RootPath);
    }

    private FileSystemBlobStore(IRecorder recorder, string rootPath)
    {
        _recorder = recorder;
        _rootPath = rootPath;
    }

    public void Dispose()
    {
        _ioLock.Dispose();
    }

    public async Task<Result<Error>> DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken)
    {
        try
        {
            var filePath = GetBlobPath(containerName, blobName);

            await _ioLock.WaitAsync(cancellationToken);
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _recorder.TraceInformation(null, "Deleted blob: {Blob} from container: {Container}", blobName, containerName);
                }
                return Result.Ok;
            }
            finally
            {
                _ioLock.Release();
            }
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex, "Error deleting blob: {Blob} from container: {Container}", blobName, containerName);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }

#if TESTINGONLY
    public async Task<Result<Error>> DestroyAllAsync(string containerName, CancellationToken cancellationToken)
    {
        try
        {
            var containerPath = GetContainerPath(containerName);

            await _ioLock.WaitAsync(cancellationToken);
            try
            {
                if (Directory.Exists(containerPath))
                {
                    Directory.Delete(containerPath, true);
                    _verifiedContainers.Remove(containerName);
                }
                return Result.Ok;
            }
            finally
            {
                _ioLock.Release();
            }
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex, "Error destroying container: {Container}", containerName);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }
#endif

    public async Task<Result<Optional<Blob>, Error>> DownloadAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            var filePath = GetBlobPath(containerName, blobName);

            await _ioLock.WaitAsync(cancellationToken);
            try
            {
                if (!File.Exists(filePath))
                {
                    return Optional<Blob>.None;
                }

                using var fileStream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true);

                await fileStream.CopyToAsync(stream, cancellationToken);

                return new Blob
                {
                    ContentType = GetContentType(filePath)
                }.ToOptional();
            }
            finally
            {
                _ioLock.Release();
            }
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex, "Error downloading blob: {Blob} from container: {Container}", blobName, containerName);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }

    public async Task<Result<Error>> UploadAsync(string containerName, string blobName, string contentType, Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            var filePath = GetBlobPath(containerName, blobName);
            var containerPath = GetContainerPath(containerName);

            await _ioLock.WaitAsync(cancellationToken);
            try
            {
                EnsureContainerExists(containerPath);

                using var fileStream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true);

                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                await stream.CopyToAsync(fileStream, cancellationToken);

                // Preservar metadata si es necesario
                File.SetCreationTimeUtc(filePath, DateTime.UtcNow);

                return Result.Ok;
            }
            finally
            {
                _ioLock.Release();
            }
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex, "Error uploading blob: {Blob} to container: {Container}", blobName, containerName);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }

    private string GetContainerPath(string containerName)
    {
        var sanitized = SanitizeContainerName(containerName);
        return Path.Combine(_rootPath, sanitized);
    }

    private string GetBlobPath(string containerName, string blobName)
    {
        var sanitizedContainer = SanitizeContainerName(containerName);
        var sanitizedBlob = SanitizeBlobName(blobName);
        return Path.Combine(_rootPath, sanitizedContainer, sanitizedBlob);
    }

    private string SanitizeContainerName(string name)
    {
        // Validación y sanitización según restricciones de Windows/Linux
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(name
            .Where(c => !invalidChars.Contains(c))
            .ToArray()).ToLowerInvariant(); // Mantener consistencia entre sistemas
    }

    private string SanitizeBlobName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(name
            .Where(c => !invalidChars.Contains(c))
            .ToArray());
    }

    private void EnsureContainerExists(string containerPath)
    {
        if (!Directory.Exists(containerPath))
        {
            Directory.CreateDirectory(containerPath);
            File.SetAttributes(containerPath, FileAttributes.Normal);
        }
    }

    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return MimeTypes.GetMimeType(extension) ?? "application/octet-stream";
    }
}