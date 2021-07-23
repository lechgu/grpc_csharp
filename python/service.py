from concurrent import futures
import statistics
import sys

import grpc

import contracts_pb2
import contracts_pb2_grpc


class Service(contracts_pb2_grpc.SvcServicer):
    def Calculate(self, request, context):  # noqa
        result = -1
        if request.op == "+":
            result = request.x + request.y
        elif request.op == "-":
            result = request.x - request.y
        elif request.op == "*":
            result = request.x * request.y
        elif request.op == "/":
            if request.y != 0:
                result = request.x // request.y
        return contracts_pb2.CalculateReply(result=result)

    def Median(self, request_iterator, context):  # noqa
        vals = []
        for temp in request_iterator:
            vals.append(temp.value)
            med = 0
            if len(vals) == 10:
                med = statistics.median(vals)
                vals = []
                yield contracts_pb2.Temperature(timestamp=temp.timestamp, value=med)


with open("../cert/service-key.pem", "rb") as f:
    key = f.read()

with open("../cert/service.pem", "rb") as f:
    cert = f.read()

creds = grpc.ssl_server_credentials(
    [
        (
            key,
            cert,
        ),
    ]
)

port = int(sys.argv[1])
svc = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
contracts_pb2_grpc.add_SvcServicer_to_server(Service(), svc)
svc.add_secure_port(f"[::]:{port}", creds)
svc.start()
print(f"service listening on port {port}...")
svc.wait_for_termination()
