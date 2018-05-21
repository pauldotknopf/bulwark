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

TODO
