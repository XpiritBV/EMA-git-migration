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

## Git Large File Storage (LFS)

Git Large File Storage (LFS) is a Git extension that improves the handling of large files by replacing them with lightweight pointers in your repository and storing the file contents on a remote server. If you're using Azure DevOps and you have large files in your repository, here's a step-by-step guide on how to set up and use Git LFS:

1.  **Installation**: Make sure you have Git LFS installed. If not, you can install it via Homebrew on macOS, apt on Ubuntu, or download it from the official website.
    
    `brew install git-lfs`
    
2.  **Initialize Git LFS**: Navigate to your repository directory and run the following command:
    
    `git lfs install`
    
3.  **Track Files**: Use the `git lfs track` command to specify which files or file patterns should be managed by LFS. For instance, to track all `.png` files, run:
    
    arduino
    
    ```arduino
    git lfs track "*.png"
    ```
    
4.  **Commit `.gitattributes`**: The above command will modify (or create if it doesn't exist) a `.gitattributes` file. This file keeps track of all patterns managed by LFS. Commit this file to ensure LFS configurations are consistent across all clones of the repository.
    
    sql
    
    ```sql
    git add .gitattributes
    git commit -m "Track PNG files with Git LFS"
    ```
    
5.  **Push to Azure DevOps**: When you push your commits to Azure DevOps, LFS-tracked files will be uploaded to Azure's LFS store.
    
    css
    
    ```css
    git push origin main
    ```
    
6.  **Clone with LFS**: When someone clones the repository, they'll need to have Git LFS installed. They can then use the standard `git clone` command, and LFS will automatically handle the large files.
    
7.  **Pulling Changes**: LFS handles downloading the large files transparently when you pull changes.
    
8.  **Azure DevOps Configuration**: Azure DevOps supports LFS out of the box, so there's no special server-side configuration needed. However, ensure that you have enough storage, and be aware of any cost implications.
    
9.  **Storage and Cost Considerations**: Azure DevOps provides some amount of free LFS storage, but there may be additional costs if you exceed the free tier. Monitor your usage and be aware of any associated costs.
    
10.  **Migrating Existing Repositories**: If you've already committed large files to your repository without LFS, you can use the `git lfs migrate` command to migrate those files to LFS. This can be a more involved process, so refer to Git LFS documentation for detailed instructions.
    

Remember, while Git LFS handles large files more efficiently than vanilla Git, it's still essential to be mindful of the repository's overall size and the number of large files you're storing. It's usually not a good idea to store extremely large datasets or binary blobs even with LFS unless absolutely necessary. Consider other storage solutions or data services if you need to store large amounts of non-code data.



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