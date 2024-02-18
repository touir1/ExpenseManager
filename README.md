# ExpenseManager
An application to manager Expenses

## RabbitMQ
the username and password defined in management_definitions.json are admin:admin (you can change them later). All the others are the same in the format user:user

Rest Api documentation: https://rawcdn.githack.com/rabbitmq/rabbitmq-server/v3.12.12/deps/rabbitmq_management/priv/www/api/index.html

to get most of the management_definitions.json config, you can use an api call: curl --user admin:admin http://127.0.0.1:15672/api/definitions

To access RabbitMQ Management: http://localhost:15672/

To access Grafana dashboards: http://localhost:3000/

Useful dashboard to integrate (json): https://grafana.com/orgs/rabbitmq
