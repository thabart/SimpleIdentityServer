import React, { Component } from "react";
import { withRouter } from 'react-router-dom';
import { translate } from 'react-i18next';
import $ from 'jquery';
window.jQuery = $;
jQuery = $;
require('jquery-ui-dist/jquery-ui.js');
require('../../elfinder/js/elfinder.full.js');

class Resources extends Component {
    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('resourcesTitle')}</h4>
                <i>{t('resourcesShortDescription')}</i>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="body">
                            	<div ref="elfinder"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
    	$(this.refs.elfinder).elfinder({
				// Connector URL
				url : 'http://localhost:5004/elfinder',

				// Callback when a file is double-clicked
				getFileCallback : function(file) {
					// ...
				},
        contextmenu: {
						cwd    : ['reload', 'back', '|', 'mkdir', 'mkfile', 'paste', 'addclient', 'mkuser', 'mkscope', '|', 'sort', '|', 'info'],
						navbar : ['open', '|', 'copy', 'cut', 'paste', 'duplicate', '|', 'rm', 'removeclient', 'rmpolicy', 'rmresource',  'rmscope', 'rmuser', '|', 'clientinfo',  'resourceinfo', 'authpolicy', 'scopeinfo', 'accessinfo', 'permissions', 'userinfo', 'umaresource' ],
            files: ['getfile', '|', 'mkdir', '|', 'copy', 'cut', 'paste', 'duplicate', '|', 'rm', 'removeclient', 'rmpolicy', 'rmscope', 'rmresource', 'rmuser', '|', 'rename', '|', 'clientinfo',  'resourceinfo', 'authpolicy', 'scopeinfo', 'accessinfo', 'info', 'permissions', 'userinfo', 'protectresource' ]
        },
				stores: [ 'openIdStore' ],
        uiOptions: {
          toolbar: [
            ['back', 'forward'],
			      ['mkdir', 'mkfile'],
			      ['rm'],
			      ['rename'],
			      ['search'],
			      ['view', 'sort'],
            ['protectresource', 'permissions']
          ]
        },
				commandsOptions: {
					clientinfo: {
						editUrl: 'https://{client_id}'
					},
					authpolicy: {
						editUrl: 'http://{authpolicy_id}'
					},
					resourceinfo: {
						editUrl: 'http://{resource_id}'
					},
					userinfo: {
						editUrl: 'http://localhost:4200/users/{user_id}'
					}
				}
			});
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(Resources));