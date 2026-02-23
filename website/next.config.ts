import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  images: {
    unoptimized: true,
  },
  basePath: "",
  trailingSlash: true,
};

export default nextConfig;
