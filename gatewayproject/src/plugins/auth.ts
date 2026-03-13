import fastifyJwt, { FastifyJWTOptions } from "@fastify/jwt";
import { FastifyReply, FastifyRequest } from "fastify";
import fp from "fastify-plugin";

export default fp<FastifyJWTOptions>(async (fastify) => {
  fastify.register(fastifyJwt, {
    secret: process.env.JWT_SECRET || "super-secret-key",
    sign: {
      expiresIn: "1h", // optional
    },
  });

  fastify.decorate(
    "authenticate",
    async function (request: FastifyRequest, reply: FastifyReply) {
      try {
        await request.jwtVerify();
      } catch (err) {
        reply.status(401).send({ message: "Request is not authenticated" });
      }
    },
  );
});

declare module "fastify" {
  interface FastifyInstance {
    authenticate(request: FastifyRequest, reply: FastifyReply): Promise<void>;
  }
}
