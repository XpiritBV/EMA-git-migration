# Bitbucket 2 Azure DevOps

## Repositories

```bash
# Clone Bitbucket Repository
git clone [YOUR_BITBUCKET_REPOSITORY_URL] [OPTIONAL_LOCAL_DIRECTORY_NAME]

# Login to Azure DevOps using az CLI
az login
az devops configure --defaults organization=[YOUR_AZURE_DEVOPS_ORG_URL] project=[YOUR_PROJECT_NAME]

# Create a New Repository in Azure DevOps
az repos create --name [YOUR_NEW_REPO_NAME]

# Push the Code to Azure DevOps:
cd [YOUR_LOCAL_DIRECTORY_NAME]
git remote add azure [YOUR_AZURE_DEVOPS_REPO_URL]
git push azure master
```
