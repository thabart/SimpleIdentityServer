import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ScopeComponent } from './common';


class OAuthScopes extends ScopeComponent {
    constructor(props) {
        super(props);
    }

    render() {
    	return (<ScopeComponent type="auth" />);
    }
}

export default translate('common', { wait: process && !process.release })(OAuthScopes);