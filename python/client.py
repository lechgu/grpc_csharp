import random
import sys

import grpc

import contracts_pb2
import contracts_pb2_grpc

host = sys.argv[1]
port = int(sys.argv[2])
with open("../cert/ca.pem", "rb") as f:
    ca_cert = f.read()
creds = grpc.ssl_channel_credentials(root_certificates=ca_cert)
channel = grpc.secure_channel(f"{host}:{port}", creds)

client = contracts_pb2_grpc.SvcStub(channel)


def do_calculator(client):
    x = int(sys.argv[3])
    op = sys.argv[4]
    y = int(sys.argv[5])
    request = contracts_pb2.CalculateRequest(x=x, y=y, op=op)
    reply = client.Calculate(request)
    print(f"The result is {reply.result}")


def generate_messages():
    ts = 1
    temp = 10
    while True:
        msg = contracts_pb2.Temperature(timestamp=ts, value=temp)
        ts += 1
        temp += random.random() - 0.5
        yield msg


def do_time_series(client):
    for msg in client.Median(generate_messages()):
        print(f"{msg.timestamp}: {msg.value}")


if len(sys.argv) > 3:
    do_calculator(client)
else:
    do_time_series(client)
