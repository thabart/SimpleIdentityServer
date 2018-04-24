import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ClientComponent } from './common';

class OAuthClients extends ClientComponent {
    constructor(props) {
        super(props);
    }

    render() {
    	return (<ClientComponent type="auth" />)
    }
}

export default translate('common', { wait: process && !process.release })(OAuthClients);