# Why SaaStack?

## Motivation

We design products and services to improve other people's lives. SaaStack is one such product.

The impetus to explore SaaStack was borne from working closely with lots and lots of start-ups, all trying to get their unique tech ideas to market, and all of whom have very real constraints on the **resources** they have (e.g. money, cash, facilities, tools, etc.) and the **expertise** to do that (e.g. people with experience that help drive success), and **time
** to get something to market that can sustain the business long term—coupled with years and years of teaching software engineering excellence to engineers teams all over the globe.

Many start-ups don't make it (some estimate up to ~90% fail to make a sustainable business), and their businesses end long before they were expected to - usually ending by running out of money.

Whilst there are no foolproof recipes that are guaranteed to work for every business in the start-up world, there are certainly many identifiable and predictable pitfalls that await everyone. Good preparation to avoid those pitfalls and resilience to be able to survive other pitfalls is what separates those who could succeed from the vast majority of those who don't survive. Despite the best-prepared or best-experienced entrepreneurs and given a product that fits a market, luck, timing, funding, and other aspects still play a critical role in whether the business survives or not.

We believe that one of the key areas that many tech startups fail in is under-engineering their evolving systems, either due to inexperience or they do it willingly but misunderstanding what the product development process is about.

By "under-engineering", we are talking about creating outcomes like:

* Failing to recognize that the software being invested in will need to live and change for the next ~5-20 years.
* Failing or ignoring to design for the future increases in complexity, as the market is explored, and market feedback governs rapid change.
* Failing to design systems that are easy to change by many other developers in the future, who will not be familiar enough with the existing code or the rationale behind its design decisions.
* Not building regression protection (test suites) into production assets, and thus spending too much time on fixing production issues, breeding more production issues.
* Not providing key capabilities for running and measuring cheap experiments.
* Not providing key measures for understanding actual user behaviors. e.g. what features actually work and features don't grow the business.
* Not designing for the desirable and inevitable scale-up of a tech product as it demonstrates success in the market.
* etc..

## What is the need for it?

Using the Value Proposition model, let's explore the value proposition, its customers, pains, and gains that SaaStack is trying to address.

> We will defer talking about how SaaStack (as a solution) will address this value proposition until later.

## Who are the customers of SaaStack?

Let's start by teasing apart the word **customer** into three parts: Buyers, Users, and Community.

**Buyers**

We believe that the "buyers" of SaaStack are going to be the startup CTOs or founding developers of new/existing tech startup businesses who are trying to build a global SaaS software product for their businesses as fast as they can.

> About CTOs: You need to know that startup CTOs (in the early days of a startup) are not the same people as the professional/executive CTOs you will find with this same title in established enterprise businesses. Same title, different jobs, different skill sets. There is a lot of confusion and [semantic diffusion](https://martinfowler.com/bliki/SemanticDiffusion.html) about what a CTO is and does. In short, a professional/executive CTO and a startup CTO are two different kinds of CTO, irrespective of the fact that you may encounter a CTO of an enterprise who was once a CTO of a startup - that can also happen, but it happens much later on.
>
> Startup CTOs, in the main, are not experienced, professional/executive CTOs. It is very rare to find a professional/executive CTO being the founding developer of a new startup - i.e. being the one building the product from the ground up. The skill sets and experience levels are wildly different. There are exceptions, of course. For example, a tech startup that is
*started* by a professional/executive CTO as a founder. In those cases, those CTOs are more than likely going to be hiring other founding developers to actually write the code for their product while they remain in the executive function of the business.
>
> In the main, a "Startup CTO" is just a fancy title for the most senior "founding developer" of the startup. It is a placeholder badge for them. They are more than likely to be just a "competent developer" with some professional experience. Experience levels may vary wildly from a novice (with a couple of years of experience writing software) to principal-level developers (with decades of experience building software). In terms of diversity of experience, regardless of years on the job, they may have very limited experience in building any durable, long-lived products from first principles, from scratch. Some may not even be used to working in teams or building software that needs to be maintained by teams of other people, and some are not experienced in building rapidly evolving distributed systems.
>
> Most founding developers will want to remain as the most "senior" technical person in their new company (as a founder, as an executive, and as a major shareholder) and remain paid the highest salary for all technical people. However, some may not want to rise up to obtain the future skill sets of an executive CTO of the company - once it scales and expands to be a stable business. Those founding developers (as current leaders of the startup business) will also unlikely want to hire more experienced technical people over the top of them, but that is a different, and future, problem.

These founding developers are the people (in the new business) who will hear about, independently investigate, assess, and judge SaaStack as a viable starting point for building their software. We believe that it is they who will make the final decision to adopt SaaStack for their startup business or not.

This person will need to quickly understand the value proposition of SaaStack and will likely be skeptical of it.

* Most will not be looking for any help in building their software, they will just do what they have been doing in their professional experience.
* Some of them will want to agree or disagree with some of the principles and practices behind SaaStack.
* Some will possibly even want to learn more about how they can change the code in the template to suit their context.

They may encounter some objections, either design decisions or principles behind it, that they can't move past.

* They will be guided by what they already know, so it will have to be a tech stack they know already.
   * They may or may not trust examples of things they are not experienced with, and some may not adopt things they won't feel they have mastered yet.
* They will be motivated by associating themselves with the latest and greatest in this technology stack (i.e. .NET).
   * They may or may not be up to speed on those things. So they will see value in someone showing them the latest and greatest.
* They will be interested to learn more about the proposed architectural styles (e.g. Event Sourcing, Microservices, Clean Architecture, etc.).
   * They may or may not be experienced in their uses either. And will be looking for evidence or opinions on why they should consider them for their context.

We believe that if the founding developer makes any kind of purchase or contribution to obtaining SaaStack, they are also going to have an expectation of having some support for adopting it, from some kind of community, where they can have questions answered and may provide feedback.

**Users**

We believe that the users of SaaStack will be all the programmers/developers/engineers of the startup who are expected to build their global SaaS product for their business.

These people will also be the ones who will be adopting the patterns, principles, and practices that SaaStack has chosen to implement. This audience will be the ones to change those patterns and extend them to suit their specific context. They will also desire some kind of support community to help them do that (given their potentially highly varied levels of experience).

We believe that they will have some skepticism and many of the same concerns that the buyer will have. However, (as employees of the startup) they are far more likely to accept the decisions already being made by the founding developer.

**Community**

We believe both audiences above will yield actionable feedback to a community that could help improve SaaStack's desirability, feasibility, and viability for reaching larger audiences.

### What are the jobs the tech startup is trying to get done?

We assume that:

- A tech startup has identified some kind of *problem* in some kind of target *market*, and this target market will (in the future) be able to sustain the growth of the business.
- They understand that there is some kind of *opportunity* out there that this problem that the market represents to them, and they accept that there is some kind of risk in competing to be a solution to that problem, that the market will choose over/with other options.
- They may or may not have a concrete solution to this problem at this stage, but must have some ideas to validate in a segment of the market.
   - We believe that many tech startups already believe that they have a winning solution in mind, and some have already convinced themselves that they only have to build the solution in their mind, and the market will come knocking on their doors, throwing money at them.
- In any case, some software will need to be designed, built, and deployed somewhere in order to test, validate, measure, or even acquire buyers/users for their new product business.
- "Making" this software is the primary job of the CTO and/or tech lead of a startup.
   - We believe that nowadays, it is imperative to research, design, build, deploy measure, and learn incrementally and iteratively to discover what really works in a market segment. These people understand that.
- Those people have to start somewhere with making some software product/prototype, and it is usually starting from scratch.
   - We believe that sometimes, it is more cost-effective to start with other low-tech solutions, low-tech tools, no-code solutions, or adapt readily available tools before a tech company accepts the risk and cost of investing in doing their own custom development. All these approaches are very viable approaches, and would likely precede the need for a solution like SaaStack.

**Early Validation:** So, our first job here is the job of getting some validated results (from a market segment) by using something more economical than making a full-blown software product of their own from scratch.

We believe that most experienced and competent CTO/leads/developers have some experience starting from scratch building some software. If they don't have that kind of experience prior, it is easily within the reach of many, simply by following some online tutorials. The outputs of those exercises are often a starting point in code to continue on with.

We also believe that all those online tutorials and examples are made to demonstrate some aspect of a technology (sometimes only a singular aspect) that the blogger/influencer, supplier, or vendor is trying to demonstrate, even if the goal is to get started on something simple (like a REST API) from scratch. We believe that few (to none) of those examples are trying to demonstrate what most of those things the developer is going to need in their first few years of building a SaaS tech product.

> We also believe that many CTO/leads/developers will be bringing with them (to their startup) knowledge of (and even copies of) previous works on which they will be basing their new works. Right or wrong, we believe that this is how the code bases of many new startups are begun by experienced CTO/leads/developers.

Whether starting from scratch or starting with proven patterns or with assets from previous projects, we believe that the CTO/lead/developers are going to be so busy spending all of their time in the early days getting something (barely working) and deployed into the market. During this time, they are not going to be focused on validating any experiments/prototypes, nor are they going to be focused on designing and supporting the validation of future experiments and prototypes.

We believe that the vast majority of the early incarnations of software that are built in the first few months of a SaaS platform, will not have any established coding patterns that are easy to adopt or easy to adapt for turning around cheap experiments. We believe that they are not going to have metrics or data capture mechanisms built into them, and getting direct feedback from the usage of the product, vital to early validation, will be difficult to engineer, with inconsistent results, if engineered at all.

Thus, we believe that having a robust platform that is easy to understand and adopt, and safe to run experiments and prototypes on, that provide immediate feedback from users on day one is going to be a valuable advantage for any early-stage startup. And we believe that CTO/leads/developers should be able to easily adapt those things to their styles, preferences, and context.

**Supportable/Maintainable:**
So we can say that the next job to be done for a CTO/lead/developer is to have an architecture that is already supportable, and maintainable (for the long term) that is open to evolve in the ways the new tech business needs, as it discovers their market. We believe that they desire the freedom to do that however they like, but at the same time, we believe that they also have high expectations that it will happen as they proceed - as part of being a software professional. Whether they explicitly call for the consideration of that or not.

We believe this because ~12 months into any SaaS startup, very few CTOs are going to easily accept that there was no logging mechanism built to diagnose errors in production at the time when a critical production issue has already occurred - that negatively affects new paying customers from using the product! It is too late by then. Supportability and maintainability will be expected to be part of the deployed software from day one. We believe these things are table stakes for any SaaS product at all stages of development.

**Modular Scalability:**

In the same vein as supportability, we believe that when market conditions change in favor of a SaaS startup, and they start increasing the demand and load on their system (with more and more users, and increased usage), that the system is going to start to slow down in one or more key areas of the system. The headroom to scale servers and services (scale up) in the cloud requires exponentially more expensive upgrades to higher scale tiers, that we believe many startups cannot afford to be paying for, especially when they are pre-revenue.

More economical options for the cloud involve selective scale-out of individual modules and workloads where existing work-loads indicate that specific modules of the system should be scaled out appropriately from others.

We believe it is also the case when fault-tolerance and high availability become necessary to keep the business going once the product starts demonstrating retention of any sort. Individual modules (or the whole system) will need to be scaled out and load-balanced appropriately.

We believe that these aspects of every SaaS product are often not considered early enough in the build of the software, due to the rush to deliver features to the market. When the time comes to address these aspects, we believe the software will need to be split up into modules and individually scaled.

However, we believe that so many developers corner themselves too early with highly coupled codebases (and tangled databases), which makes this play either impossible (and thus very expensive to re-engineer) or requires high re-engineering costs to remediate, in too short and immediate timeframes.

We believe that no startup has an abundance of this time or resources, especially pre-revenue. We also believe that these issues will surface just at the (wrong) time, just when the SaaS product needs to demonstrate its scalability to the demands of a larger market, as the business starts to demonstrate increases in traction and growth. We believe that any SaaS business (at this stage) will have little tolerance of this negative outcome and will take strong (if not extreme) measures to ensure it does not occur again in the future.

**Others**: TBD?

### What are the pains that annoy tech startups in the early stages?

(negative outcomes)

* Initial expense (in time) to get the first release to market
* The time it takes (rate) to add new features
* Slow decline in the initial rates to get new features to market down the track
* Production issues begin to be a growing expense. Erode trust between technical and non-technical founders
* Production issues take more time than responding to the market, which frustrates non-technical founders
* Support/CS personnel are required to be hired to stem the churn of early adopters, and the early majority
* Building ineffective features that produce no valuable product outcomes ([AARRR](https://www.productplan.com/glossary/aarrr-framework/)).
* The increased time to onboard less experienced technical people from getting productive with the codebase
* Time to build the same old features over and over again from scratch
* Time and complexity to build basic integrations to 3rd parties

### How do tech startups measure the success of a job well done in the early stages?

(positive outcomes)

* Being able to run cheap experiments to test the problem being solved
* Being able to run some experiments in the software product being built
* Having a robust and durable platform to build early on
* Establishing measurement early on
* Not spending time on things that should already be there
* Being able to scale the pieces that need scaling when (or before) they reach capacity.
* Leveraging and building upon known trusted principles and practices from day one.
* Leveraging the diligence and credibility of a dedicated community
* etc.

## Why me?

TBD

- been doing this for a long time, in the trenches with early-stage startups
- used different tools

- have been teaching this stuff to companies that work with early-stage startups

## Why now?

TBD

platforms and tooling are still getting better.