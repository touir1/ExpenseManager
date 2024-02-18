# Use the official RabbitMQ image from Docker Hub
FROM rabbitmq:3.9.9-management

# Expose the RabbitMQ port
EXPOSE 5672 15672

# Set the default user and password for RabbitMQ management plugin
ENV RABBITMQ_DEFAULT_USER=admin
ENV RABBITMQ_DEFAULT_PASS=admin

# Copy custom configuration file into the container
COPY ./RabbitMQConfig/rabbitmq.conf /etc/rabbitmq

# Create volume directories for data and logs
RUN mkdir -p /var/lib/rabbitmq /var/log/rabbitmq

# Set permissions for RabbitMQ to access the volume directories
RUN chown -R rabbitmq:rabbitmq /var/lib/rabbitmq /var/log/rabbitmq

# Define volumes for data and logs
VOLUME /var/lib/rabbitmq /var/log/rabbitmq /etc/rabbitmq

# Define the command to run RabbitMQ server
CMD ["rabbitmq-server"]