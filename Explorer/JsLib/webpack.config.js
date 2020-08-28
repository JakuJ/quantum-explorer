const path = require("path");

const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');

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
            }, {
                test: /\.css$/,
                use: ['style-loader', 'css-loader']
            }, {
                test: /\.ttf$/,
                use: ['file-loader']
            }

        ]
    },
    output: {
        path: path.resolve(__dirname, '../wwwroot/js'),
        filename: "library.js",
        library: "Library"
    },
    plugins: [
        new MonacoWebpackPlugin()
    ]
};
