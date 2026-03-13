import { FastifyPluginAsync } from "fastify";

const orderRoute: FastifyPluginAsync = async (fastify, _opts) => {
  fastify.get("/orders", async () => {
    return { message: "order route is working" };
  });
};

export default orderRoute;
