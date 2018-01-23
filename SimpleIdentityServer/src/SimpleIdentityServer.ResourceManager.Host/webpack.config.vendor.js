const path = require('path');
const bundleOutputDir = './wwwroot/dist';
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

var config = (env) => {
    const isDevBuild = !(env && env.prod);
    return [{
        entry: ["jquery", "bootstrap", "tether"],
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: "vendor.js",
            publicPath: '/dist/'
        },
        plugins: [
            new webpack.ProvidePlugin({
                $: "jquery",
                jQuery: "jquery",
                "window.jQuery": "jquery"
            }),
            new webpack.ProvidePlugin({
                tether: "tether",
                Tether: "tether",
                "window.Tether": "tether"
            })
        ],
    }];
};
module.exports = config;