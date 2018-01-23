const path = require('path');
const bundleOutputDir = './wwwroot/dist';
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

var config = (env) => {
  const isDevBuild = !(env && env.prod);
  return [{	  
      context: __dirname + "/ClientApp",
      entry: { 'main': __dirname + "/ClientApp/main.js" },
	  output: {
        filename: '[name].js',
		path: path.join(__dirname, bundleOutputDir),
        publicPath: '/dist/'
	  },
	  module: {
		loaders: [ 
			{ test: /\.js$/, exclude: /node_modules/, loader: 'babel-loader', query: { presets: ['react', 'env'] } },
			{ test: /\.css$/, use: isDevBuild ? ['style-loader', 'css-loader'] : ExtractTextPlugin.extract({ use: 'css-loader?minimize' }) },
			{ test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' }
		],
      }
  }];
};
module.exports = config;