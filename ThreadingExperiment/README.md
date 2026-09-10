# SchoolAccount.GovNotify Notification Theading Experiment

Latest version can be found here : [School Account Confluence](https://dfedigital.atlassian.net/wiki/spaces/SCHO/pages/edit-v2/6523584523)

## Hangfire vs Azure Container App Jobs

Here is a introduction look at the benefits on Hangfire vs Azure Container App Jobs (App Jobs)

### Hangfire

#### Pros
- Per-email retry for free. Hangfire retries a failed job 10 times by default with increasing delays, and a job that exhausts its retries lands in a Failed state you can requeue from the dashboard. That is exactly the failure handling in step 7, already built.
- The dashboard is a genuine ops asset. Support can see the last run, what failed and why, and hit retry without a deployment.
- Cron honours a TimeZoneInfo, so Europe/London at 08:00 stays 08:00 through BST.
- [DisableConcurrentExecution] solves the overlap problem in one attribute.
- No new infrastructure if you already have a web app running.

#### Con
- It needs a host that never sleeps. On App Service that means Always On and a non-free tier; we're paying 24/7 for a job that runs for seconds a day - guess you could close it down on the evening as we know nothing will need to run during that period?
- It creates its own schema (about ten tables) in a database. In DfE that usually means a DBA conversation and a migration story.
- The dashboard is unauthenticated by default and will be picked up in a pen test if you deploy it that way. Needs an authorisation filter wired to DfE Sign-in or restricted to an internal path.
- Background jobs sharing a process with web requests means a bad run competes for the same CPU and connection pool as your users. 
- The state lives in SQL, so recovery from a corrupted or half-migrated Hangfire schema is its own operational problem.

### App Jobs

#### Pros
- Scales to zero. You pay for the seconds it runs.
- No dashboard to secure, no shared process with the web app. Managed identity to SQL and to Key Vault, same environment and networking as your other container apps.
- The job is the deployment artefact. Same image, same pipeline, and it can be ran manually with az containerapp job start to reproduce a bad run.
- Failure is a clean signal: non-zero exit, visible in execution history, alertable in Azure Monitor.

#### Cons
- Cron expressions are evaluated in UTC, with no time zone option. An 08:00 job is 09:00 local through summer. You either accept the drift, run hourly and gate in code, or change the expression twice a year. More info at Tomodahinata
- Retry is all-or-nothing. replicaRetryLimit re-runs the entire container, so a failure at email 90 of 100 replays all 100. Your job body has to be idempotent, or at least tolerant of duplicates.
- Replica timeout takes precedence over the retry limit, so a job that hangs shows as DeadlineExceeded rather than a useful error. Set the timeout generously above expected runtime. More info at Github
- Execution history holds about 100 runs. Anything longer-term has to go to Log Analytics or your own run table.
- No ops UI. Retrying a specific email means someone runs a manual job execution with arguments, or you build a small admin page.

### The Plan

The flow for the work would do the following:
1. Get the json/excel sheet on who has signed up to the beta with their email and LAEStab
2. Get all the time the last we ran against a given LAEStab, if not ran skip
3. Connect to the ledger requesting all records that have changed from date (skip date argument if never ran before) against a list of LAEStabs & collection name
4. Optional, because we are only update based on status code the hash is based off other properties so we probably will need to get the second to last row to do a fair comparison
5. Add a thread item for each change
   1. If successful, add a row to the last ran against table
   2. If failed, add a failure log item or attach a failed notice to the last ran row
   3. If a email related failure try and retry x amount of times
6. If the app fails exit and report
7. Repeat next day

### Which one fits?
A thing which makes Hangfire a head starter is all built in error handling and parallelism within the package out the box. Whilst App Jobs will have to be hand crafted everything, we could keep it simple where the main thread deals with the orchestration of deciding who gets what and then create a .NET Parallel pool to work emails across threads blocking the thread via a TokenBucketRateLimiter.

Quickly summarising this up is it Cost vs Simplicity. Hangfire is simple but requires a constantly running container; whilst, App Jobs is more code/complexity but long term a lot cheaper.

Another idea worth being considered and probably rejected is: a scheduled job that only computes the send list and drops one Service Bus message per email, consumed by an event-driven job. That gives per-message retry and dead-lettering with no Hangfire schema. It's the right answer at tens of thousands of emails. For a beta list it's over-built.