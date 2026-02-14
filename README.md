# ExpenseManager
An application to manager Expenses

## RabbitMQ
Credentials are configured via environment variables and seeded definitions.
Avoid committing plaintext usernames/passwords. Use placeholders or env vars.

Example (replace with your own values):
curl --user ${RABBITMQ_USER}:${RABBITMQ_PASSWORD} http://127.0.0.1:15672/api/definitions

Rest Api documentation: https://rawcdn.githack.com/rabbitmq/rabbitmq-server/v3.12.12/deps/rabbitmq_management/priv/www/api/index.html

To get most of the management_definitions.json config, you can use the API:
curl --user <username>:<password> http://127.0.0.1:15672/api/definitions

To access RabbitMQ Management: http://localhost:15672/

To access Grafana dashboards: http://localhost:3000/

Useful dashboard to integrate (json): https://grafana.com/orgs/rabbitmq
