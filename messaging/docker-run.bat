@echo off

docker build -t rabbitmq-expenses:latest .

docker run -d --name rabbitmq-expenses ^
  -v rabbitmq_conf:/etc/rabbitmq ^
  -v rabbitmq_data:/var/lib/rabbitmq ^
  -v rabbitmq_logs:/var/log/rabbitmq ^
  rabbitmq-expenses