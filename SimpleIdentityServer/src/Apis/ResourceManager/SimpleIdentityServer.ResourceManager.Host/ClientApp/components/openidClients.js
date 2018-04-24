import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ClientComponent } from './common';

class OpenidClients extends Component {
    constructor(props) {
        super(props);
    }

    render() {
    	return (<ClientComponent type="openid" />)
    }
}

export default translate('common', { wait: process && !process.release })(OpenidClients);