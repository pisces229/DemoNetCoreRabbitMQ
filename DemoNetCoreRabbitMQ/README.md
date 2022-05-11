## RabbitMQ[https://hub.docker.com/_/rabbitmq]

`docker pull rabbitmq:managemen`

`docker run --name myrabbitmq -p 15672:15672 -p 5672:5672 -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=123 -d rabbitmq:management`

