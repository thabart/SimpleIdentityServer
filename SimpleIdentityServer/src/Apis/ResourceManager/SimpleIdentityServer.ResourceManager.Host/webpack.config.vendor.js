const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = (env) => {
    const extractCSS = new ExtractTextPlugin('vendor.css');
    const isDevBuild = !(env && env.prod);
    return [{
        stats: { modules: false },
        resolve: {
            extensions: ['.js']
        },
        module: {
            rules: [
                { test: /\.(png|woff|woff2|eot|ttf|svg|gif)(\?|$)/, use: 'url-loader?limit=100000' },
                { test: /\.css(\?|$)/, use: extractCSS.extract([isDevBuild ? 'css-loader' : 'css-loader?minimize']) }
            ]
        },
        entry: {
            vendor: ['react', 'react-dom', 'react-router-dom', 'jquery', 'popper.js', 'bootstrap/dist/css/bootstrap.css', 
					'react-table/react-table.css', 'jquery-ui/themes/base/all.css', 'codemirror/lib/codemirror.css', 
					'react-datepicker/dist/react-datepicker.css', 'roboto-fontface/css/roboto/roboto-fontface.css',
					'material-icons/iconfont/material-icons.css', './elfinder/css/elfinder.full.css'],
        },
        output: {
            path: path.join(__dirname, 'wwwroot', 'dist'),
            publicPath: 'dist/',
            filename: '[name].js',
            library: '[name]_[hash]',
        },
        plugins: [
            extractCSS,			
			new CopyWebpackPlugin([
				{ from: 'elfinder/img', to: '../img' }
			], {}),
            new webpack.ProvidePlugin({ $: 'jquery', jQuery: 'jquery', 'window.jQuery': 'jquery', 'window.$' : 'jquery' }), // Maps these identifiers to the jQuery package (because Bootstrap expects it to be a global variable)
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            })
        ].concat(isDevBuild ? [] : [
            new webpack.optimize.UglifyJsPlugin()
        ])
    }];
};
