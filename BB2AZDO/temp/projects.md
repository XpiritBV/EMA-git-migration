#


## Authentication
First, you'll need to authenticate. You can use basic authentication (username and password) or App passwords (recommended) for this purpose.

To generate an App password:

* Log in to Bitbucket.
* Go to Bitbucket settings (avatar at the bottom left corner).
* Under "Access management", click on "App passwords".
* Click "Create app password".
* Give it a name and choose the permissions you want. For listing repositories, you'll need "Read" permission for repositories.

## Retrieve list

```bash
curl -u YOUR_USERNAME:YOUR_PASSWORD https://YOUR_BITBUCKET_SERVER_URL/rest/api/1.0/projects
```

