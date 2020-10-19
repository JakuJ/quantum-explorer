const path = require('path');

const { BundleAnalyzerPlugin } = require('webpack-bundle-analyzer');

const { CleanWebpackPlugin } = require('clean-webpack-plugin')
const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin')

module.exports = {
    mode: 'production',
    entry: {
        library: path.resolve(__dirname, 'library.js'),
    },
    output: {
        path: path.resolve(__dirname, '../wwwroot/js'),
        publicPath: '/js/',
        filename: "[name].bundle.js",
        library: "Library"
    },
    module: {
        rules: [
            {
                test: /\.jsx?$/,
                exclude: /node_modules/,
                use: ['babel-loader']
            }, 
            // --- Loaders for monaco ---
            {
                test: /\.css$/,
                include: path.join(__dirname, "node_modules", "monaco-editor"),
                use: ["style-loader", "css-loader"],
            },
            {
                test: /\.ttf$/,
                include: path.join(__dirname, "node_modules", "monaco-editor"),
                use: ["file-loader"],
            },
        ]
    },
    node: {
        net: 'mock'
    },
    resolve: { alias: { 'vscode': require.resolve('monaco-languageclient/lib/vscode-compatibility') } },
    output: {
        path: path.resolve(__dirname, '../wwwroot/js'),
        publicPath: '/js/',
        filename: "library.js",
        library: "Library"
    },
    plugins: [
        new CleanWebpackPlugin(),
        new MonacoWebpackPlugin({
            languages: [],
            features: [
                "!accessibilityHelp",
                "!codeAction",
                "!fontZoom",
                "!iPadShowKeyboard",
                "!toggleHighContrast",
                "!toggleTabFocusMode",
            ],
        }),
        // new BundleAnalyzerPlugin() // uncomment to see packed assets' size distribution 
    ]
};
