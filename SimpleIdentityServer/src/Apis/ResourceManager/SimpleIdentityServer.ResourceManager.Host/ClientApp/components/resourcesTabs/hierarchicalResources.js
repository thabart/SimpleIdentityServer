import React, { Component } from "react";
import { withRouter } from 'react-router-dom';
import { translate } from 'react-i18next';
import $ from 'jquery';
import Constants from '../../constants';
window.jQuery = $;
jQuery = $;
require('jquery-ui-dist/jquery-ui.js');
require('../../../elfinder/js/elfinder.full.js');

class HierarchicalResources extends Component {
    render() {
        var self = this;
        const { t } = self.props;
        return (
            <div className="card">
                <div className="header">
                    <h4 style={{display: "inline-block"}}>{t('hierarchicalResources')}</h4>
                </div>
                <div className="body" style={{ overflow: "visible" }}>
                    <div ref="elfinder"></div>
                </div>
            </div>
        );
    }

    componentDidMount() {
    	$(this.refs.elfinder).elfinder({
				url : Constants.apiUrl + '/elfinder',

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
					}
				}
			});
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(HierarchicalResources));