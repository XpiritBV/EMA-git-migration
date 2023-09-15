# Manual

## Git migation

In the command line navigate to the repository folder and execute the following commands

    git remote rm origin
    git remote add {new url}

## Git meta data

To set the author name and email, use following commands

    git config --global user.name "Your Name"
    git config --global user.email you@example.com

Note: An action in git has both an user and an author. The author can be configured, the user is detirmend by your credentials towards Azure Devops

## Migration tool

    Usage: migrationtool.exe migrate <Project> <Repository> [options]

    Arguments:
    Project                      * Project key
    Repository                   * Repository slug

    Options:
    -v|--version                 Show version information.
    -h|--help                    Show help information.
    -b|--branch                  [Aditional branches to migrate]
    -tp|--target-prefix          [Prefix for target project]
    -t|--target                  [Target project slug]
    -tr|--target-repository      [Target repository slug]
    -s|--skip-if-target-exists   [Skip if target repository exists]
    -pr|--pull-requests          [Migrate pull requests]
                                 Default value is: True.
    -pc|--pull-request-comments  [Migrate pull request comments]
                                 Default value is: True.

### Examples

    migrationtool.exe migrate MIG workrepo

This will migrate the MIG project to MIG on azure devops, only doing the master/main branch

    migrationtool.exe migrate MIG workrepo -tp:TMP 

This will migrate the MIG project to TMP-MIG on azure devops, only doing the master/main branch

    migrationtool.exe migrate MIG workrepo -t:OTHER 

This will migrate the MIG project to OTHER on azure devops, only doing the master/main branch

    migrationtool.exe migrate workrepo -b:develop -b:release/1.0.0 MIG 

This will migrate the MIG project to MIG on azure devops, migrating main/master, develop and release/1.0.0 repository