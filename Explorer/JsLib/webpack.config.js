const path = require("path");

const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');

module.exports = {
    entry: __dirname,
    module: {
        rules: [
            {
                test: /\.(js|jsx)$/,
                exclude: /node_modules/,
                use: {
                    loader: "babel-loader"
                }
            }, {
                test: /\.css$/,
                use: ['style-loader', 'css-loader']
            }, {
                test: /\.ttf$/,
                use: ['file-loader']
            }, {
                test: /\.wasm$/,
                loader: "file-loader",
                type: "javascript/auto",
            }

        ]
    },
    output: {
        path: path.resolve(__dirname, '../wwwroot/js'),
        publicPath: '/js/',
        filename: "library.js",
        library: "Library"
    },
    plugins: [
        new MonacoWebpackPlugin()
    ]
};
