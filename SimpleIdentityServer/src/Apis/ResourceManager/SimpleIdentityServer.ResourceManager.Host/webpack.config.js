const path = require('path');
const bundleOutputDir = './wwwroot/dist';
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

var config = (env) => {
  const isDevBuild = !(env && env.prod);
  var baseUrl = process.env.BASE_URL;
  var evtUrl = process.env.EVT_SOURCE_URL;
  var apiUrl = process.env.API_URL;
  if (!baseUrl) {
	baseUrl = '"http://localhost:64950"';
  }
  
  if (!evtUrl) {
	evtUrl = '"http://localhost:60002"';
  }
  
  if (!apiUrl) {
      apiUrl = '"http://localhost:60005"';
  }
  
  return [{	  
      context: __dirname + "/ClientApp",
      entry: { 'main': __dirname + "/ClientApp/main.js" },
	  output: {
        filename: '[name].js',
		path: path.join(__dirname, bundleOutputDir),
        publicPath: '/dist/'
	  },
	  plugins: [
		new webpack.DefinePlugin({
			'process.env': {
				'IS_LOG_DISABLED' : process.env.IS_LOG_DISABLED,
				'IS_TOOLS_DISABLED': process.env.IS_TOOLS_DISABLED,
				'IS_RESOURCES_DISABLED': process.env.IS_RESOURCES_DISABLED,
				'IS_CONNECTIONS_DISABLED': process.env.IS_CONNECTIONS_DISABLED,
				'IS_SETTINGS_DISABLED': process.env.IS_SETTINGS_DISABLED,
				'IS_CACHE_DISABLED': process.env.IS_CACHE_DISABLED,
				'IS_MANAGE_DISABLED': process.env.IS_MANAGE_DISABLED,
				'BASE_URL': baseUrl,
				'EVT_SOURCE_URL': evtUrl,
				'API_URL': apiUrl
			}
		})
	  ],
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