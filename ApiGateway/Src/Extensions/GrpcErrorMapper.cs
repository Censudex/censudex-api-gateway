using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Src.Extensions
{
    public static class GrpcErrorMapper
    {
        public static IActionResult MapGrpcErrorToHttp(RpcException ex)
        {
            var statusCode = ex.StatusCode switch
            {
                StatusCode.OK => 200,
                StatusCode.NotFound => 404,
                StatusCode.InvalidArgument => 400,
                StatusCode.Unauthenticated => 401,
                StatusCode.PermissionDenied => 403,
                StatusCode.AlreadyExists => 409,
                StatusCode.FailedPrecondition => 412,
                StatusCode.Internal => 500,
                StatusCode.Unavailable => 503,
                _ => 500
            };

            return new ObjectResult(new
            {
                error = ex.Status.Detail,
                code = ex.StatusCode.ToString()
            })
            {
                StatusCode = statusCode
            };
        }
    }
}