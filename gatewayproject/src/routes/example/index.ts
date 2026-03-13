import { FastifyPluginAsync } from "fastify";

const example: FastifyPluginAsync = async (fastify, _opts): Promise<void> => {
  fastify.get("/example", async () => {
    return { example: true };
  });
};

export default example;
