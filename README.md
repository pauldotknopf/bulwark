# Bulwark

An implementation if GitHub's CODEOWNERS file, but for GitLab.

[![Bulwark](https://img.shields.io/nuget/v/Bulwark.svg?style=flat-square&label=Bulwark)](http://www.nuget.org/packages/Bulwark/)
[![Build Status](https://travis-ci.com/pauldotknopf/bulwark.svg?branch=develop)](https://travis-ci.com/pauldotknopf/bulwark)

## Details

The `CODEOWNERS` file acts exactly as `.gitignore`. Similary, the file can also be nested in child directories to add/remove inherited users.

```config
* pauldotknopf
*.txt someoneelse

# You can also remove users from previously inherited matches.
*.pdf !pauldotknopf

```

## Installation

1. Run the web hook server. Example `docker-compose.yml` file [here](build/docker/example/docker-compose.yml). Configurable options [here](todo).
   * At a bare minimum, you should have the following configured for Bulwark to properly communicate with GitLab.
   ```
   {
     "GitLab": {
       "AuthenticationToken": "your-auth-token"
      },
   }
   ```
   This configuration should go in a `config.json` file in the working directory of the running Bulwark instance.
2. On GitLab under `Project > Settings > Integrations`, add a web hook that points to `https://your-bulwark-instance.com/gitlab` and tick the following:
   * [x] Push events
   * [x] Merge request events
3. On GitLab under `Project > Settings > General`, tick following:
   * [x] Merge request approvals
   * [x] Can override approvers and approvals required per merge request
   * [x] Remove all approvals in a merge request when new commits are pushed to its source branch (optional)

That's it. Submit a pull request with a CODEOWNERS file and watch users get automatically assigned as reviewers.

## More options

### Message queue

**Defaults**:

```
{
  "MessageQueue": {
    "Type": "Sqlite",
    "SqlLiteDBLocation": "sqlite.db",
    "RabbitMqHost": null,
    "RabbitMqUsername": null,
    "RabbitMqPassword": null,
    "RabbitMqPort": 5672
  }
}
```

**Details**:

* `"Type"`:
  * `"Sqlite"` - The default method. New messages are stored in the database and a worker thread (or another process) consumes them.
  * `"RabbitMq"` - Use an external RabbitMQ server to store the message.

### GitLab

**Defaults**:

```
{
  "GitLab": {
    "Enabled": true,
    "ServerUrl": "https://gitlab.com/",
    "AuthenticationToken": null,
    "TargetBranchesFilter": null,
    "AutoMergePullRequests": false,
    "MergeCommitMessage": null,
    "ShouldRemoveSourceBranch": null
  }
}
```

**Details**:

* `"ServerUrl"`: You can point this to ```gitlab.com``` or your own hosted GitLab instance.
* `"AuthenticationToken"`: Generate this from your account settings.
* `"TargetBranchesFilter"`: A regular expression to match against branches you wish to process. You may want to set this to `"master"`.
* `"AutoMergePullRequests"`: If all the required approvers have approved, you can configure Bulwark to auto merge the merge request. You might want to update your `Project > Settings > Repository > Protected Branches` settings to only authorize Bulwark to merge your merge requests to your desired branch.
* `"MergeCommitMessage"`: Self explanatory, empty if you want GitLab to auto-generate a merge commit message.
* `"ShouldRemoveSourceBranch"`: Self explanatory, empty if you want to let GitLab to use the configured value for the merge request.

### Repository cache

**Defaults**:

```
{
  "RepositoryCache": {
    "RepositoryCacheLocation": "repository-cache"
  }
}
```

**Details**:

* `"RepositoryCacheLocation"`: The directory that repositories will be cloned to do internal diffs between commits.
