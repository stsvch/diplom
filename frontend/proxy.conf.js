const backendUrl = (process.env.BACKEND_URL || 'http://localhost:5000').trim();

module.exports = {
  '/api': {
    target: backendUrl,
    secure: false,
    changeOrigin: true,
    logLevel: 'info',
  },
  '/hubs': {
    target: backendUrl,
    secure: false,
    changeOrigin: true,
    ws: true,
  },
  '/swagger': {
    target: backendUrl,
    secure: false,
    changeOrigin: true,
    logLevel: 'info',
  },
};
