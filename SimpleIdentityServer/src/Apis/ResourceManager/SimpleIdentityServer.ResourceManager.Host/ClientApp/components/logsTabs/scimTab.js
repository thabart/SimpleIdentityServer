import React, { Component } from "react";
import { LogTables } from '../common';
import { translate } from 'react-i18next';

class ScimTab extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const { t } = this.props;
        return (<LogTables logTitle={t('scimLogsTitle')} errorTitle={t('scimErrorsTitle')} type='scim' />);
    }
}

export default translate('common', { wait: process && !process.release })(ScimTab);