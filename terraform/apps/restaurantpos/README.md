# RestaurantPOS App Stack (skeleton)
Placeholder for a future app using shared modules from `../../modules`.

- Env/region stacks will live under `envs/<env>/<region>/` (backend.tf + *.tfvars + main.tf).
- App composition will live under `app_stack/` (ECS services, ALB, DB/cache/S3 as needed).
- Images will come from the RestaurantPOS app Dockerfiles and be referenced in tfvars.
