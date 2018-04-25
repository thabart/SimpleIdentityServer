import React, { Component } from "react";
import { translate } from 'react-i18next';
import { LogTables } from '../common';

class AuthorizationTab extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const { t } = this.props;
        return (<LogTables logTitle={t('authLogsTitle')} errorTitle={t('authErrorsTitle')} type='auth' />);
    }
}

export default translate('common', { wait: process && !process.release })(AuthorizationTab);