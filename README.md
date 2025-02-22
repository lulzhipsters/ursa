# Ursa

![CI](https://github.com/lulzhipsters/ursa/actions/workflows/ci.yml/badge.svg?branch=main)

Opaque bearer token auth provider for use in a "Through Auth" pattern by reverse proxies such as:
- [Ory Oathkeeper](https://www.ory.sh/docs/oathkeeper)
- [Traefik](https://doc.traefik.io/traefik/middlewares/http/forwardauth/)
- [Caddy](https://caddyserver.com/docs/caddyfile/directives/forward_auth)

Note that at this point it is being developed for and tested for Oathkeeper