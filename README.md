# Starrysky

![Logo](./icon.png)

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/alexmg/Starrysky/blob/main/LICENSE)
[![Open in VS Code](https://img.shields.io/badge/Open%20in%20VS%20Code-blue?logo=visualstudiocode)](https://open.vscode.dev/alexmg/Starrysky)

Starrysky is a .NET tool that posts a random GitHub star to Bluesky. It helps you share interesting repositories you have starred on GitHub with your Bluesky followers.

## Installation

To install Starrysky as a global .NET tool, run the following command:

```sh
dotnet tool install --global Starrysky
```

## Usage

To use Starrysky, run the following command:

```sh
starrysky [OPTIONS]
```

## Synopsis

```text
starrysky [-h|--help]
    [-t|--token <TOKEN>]
    [--handle <HANDLE>]
    [-p|--password <PASSWORD>]
    [-d|--dry-run {true|false}]
    [-c|--caching {true|false}]
    [-f|--footer {true|false}]
    [--header <HEADER>]
```

## Options

- `-h|--help`
  Prints help information.

- `-t|--token` (default: `null`)

  Token for access to the GitHub API. This can also be set in the `Starrysky__GitHubToken` environment variable.

- `--handle` (default: `null`)

  Handle of the Bluesky account. This can also be set in the `Starrysky__BlueskyHandle` environment variable.

- `-p|--password` (default: `null`)

  Password for the Bluesky account. This can also be set in the `Starrysky__BlueskyPassword` environment variable.

- `-d|--dry-run` (default: `false`)

  Prints the post to the console without posting to Bluesky or saving history.

- `-c|--caching` (default: `false`)

  Enables caching of the starred GitHub repositories retrieved from the GitHub API. This is useful during development and testing if you have a large number of starred repositories. The cached API results are stored in a `repos.json` file in the working directory.

- `--footer` (default: `true`)

  Include a footer in the post with a link to this project. This is intended to allow others to discover the project and hopefully use it themselves.

- `--header` (default: `null`)

  A custom header to use for the post instead of the standard header text.

## GitHub Action

You can run the Starrysky command line tool in a GitHub Action. This is a convenient way of automating your posts without incurring any hosting costs. Here is an example of a workflow that posts a random GitHub star to Bluesky daily:

```yml
name: Daily Bluesky Post

on:
  schedule:
    - cron: '0 0 * * *'
  workflow_dispatch:
    inputs:
      dryRun:
        required: false
        default: false
        description: "Execute a dry-run that does not post to Bluesky"
        type: boolean

jobs:
  post:
    name: Post to Bluesky
    runs-on: ubuntu-latest
    permissions:
      contents: write
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4.2.2
        with:
          fetch-depth: 0
          filter: tree:0

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4.2.0
        with:
          dotnet-version: '9.0.x'

      - name: Install global tool
        run: dotnet tool install --global Starrysky

      - name: Execute global tool
        run: >
          starrysky \
            --token ${{ secrets.GITHUB_TOKEN }} \
            --handle ${{ secrets.BLUESKY_HANDLE }} \
            --password ${{ secrets.BLUESKY_PASSWORD }}
```

To use this workflow ensure that you have added [secrets](https://docs.github.com/en/actions/security-for-github-actions/security-guides/using-secrets-in-github-actions) to your repository for the `GITHUB_TOKEN`, `BLUESKY_HANDLE`, and `BLUESKY_PASSWORD`.

The `GITHUB_TOKEN` should be a [fine-grained token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-fine-grained-personal-access-token) and should be restricted to accessing the repository that contains the GitHub workflow.

Repository permissions:

- Read access to metadata
- Read and Write access to code

User permissions:

- Read access to starring

These are permissions are required to query the GitHub API for your starred repositories and to commit the `history.json` file containing a list of repositories that have been posted to Bluesky.

You can generate a Bluesky [App Password](https://bsky.app/settings/app-passwords) for the `BLUESKY_PASSWORD` secret.

Update the `cron` expression to a time (UTC) that works best for your target audience.

The workflow includes the `workflow_dispatch` event to allow for manual triggering from the GitHub frontend. When triggering a manual run of the workflow an input is provided to optionally perform a dry-run.

## Credits

Icon made by [meaicon](https://www.flaticon.com/authors/meaicon) from [www.flaticon.com](https://www.flaticon.com/)
