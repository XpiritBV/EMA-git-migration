# Bitbucket 2 Azure DevOps

## Pull Requests

### General

```bash
# Export Pull Requests from Bitbucket
curl -u USERNAME:PASSWORD https://api.bitbucket.org/2.0/repositories/USERNAME/REPO_NAME/pullrequests

# Create Pull Requests in Azure DevOps
curl -X POST -H "Content-Type: application/json" -H "Authorization: Basic BASE64_ENCODED_PERSONAL_ACCESS_TOKEN" -d '{
    "sourceRefName": "refs/heads/SOURCE_BRANCH",
    "targetRefName": "refs/heads/TARGET_BRANCH",
    "title": "PR_TITLE",
    "description": "PR_DESCRIPTION"
}' https://dev.azure.com/YOUR_ORG/YOUR_PROJECT/_apis/git/repositories/YOUR_REPO/pullrequests?api-version=6.0
```

### Comments

```bash
# Export Pull Request Comments from Bitbucket
curl -u USERNAME:PASSWORD https://api.bitbucket.org/2.0/repositories/USERNAME/REPO_NAME/pullrequests/PULL_REQUEST_ID/comments


# Import Pull Request Comments to Azure DevOps
curl -X POST -H "Content-Type: application/json" -H "Authorization: Basic BASE64_ENCODED_PERSONAL_ACCESS_TOKEN" -d '{
    "parentCommentId": 0,
    "content": "YOUR_COMMENT_CONTENT"
}' https://dev.azure.com/YOUR_ORG/YOUR_PROJECT/_apis/git/repositories/YOUR_REPO/pullrequests/PULL_REQUEST_ID/threads?api-version=6.0
```

The comments you add to Azure DevOps will have the timestamp of when you added them, not when they were originally made in Bitbucket. Also, they will be authored by the user associated with the Azure DevOps PAT, not the original Bitbucket user. If preserving the original author and timestamp is crucial, you might include this information in the migrated comment's text.

#### Nested

Ensure that the parent-child relationship between comments

#### Attachments

```bash
# Retrieve Attachments from Bitbucket:
curl -u USERNAME:PASSWORD -O [ATTACHMENT_URL]

# Upload Attachments to Azure DevOps
curl -X POST -H "Content-Type: application/octet-stream" -H "Authorization: Basic BASE64_ENCODED_PERSONAL_ACCESS_TOKEN" --data-binary @[PATH_TO_ATTACHMENT] https://dev.azure.com/YOUR_ORG/YOUR_PROJECT/_apis/wit/attachments?fileName=YOUR_FILENAME&api-version=6.0

```

Once you've uploaded the attachment to Azure DevOps, you can use the returned URL to link the attachment in your pull request comments. When creating or updating a comment in Azure DevOps, you would include the Azure DevOps attachment URL in the comment's content.

Remember to consider the markdown or HTML format used in the pull request comments to properly embed or link the attachment.