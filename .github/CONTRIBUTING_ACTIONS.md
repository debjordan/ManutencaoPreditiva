GitHub Actions and secrets

- `DOCKER_USERNAME` and `DOCKER_PASSWORD`: optional, required if you want the `release.yml` workflow to push images to a registry.
- `GH_TOKEN`: used by some release actions to create GitHub releases (already handled by the action in `release.yml`).

Set secrets at: https://github.com/<owner>/<repo>/settings/secrets/actions
