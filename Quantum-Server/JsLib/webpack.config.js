const path = require("path");

module.exports = {
  mode: "production",
  entry: __dirname,
  module: {
    rules: [
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader"
        }
      }
    ]
  },
  output: {
    path: path.resolve(__dirname, '../wwwroot/js'),
    filename: "library.js",
    library: "Library"
  }
};
