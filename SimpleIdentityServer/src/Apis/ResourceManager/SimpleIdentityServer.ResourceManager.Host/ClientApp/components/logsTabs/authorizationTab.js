import React, { Component } from "react";
import { translate } from 'react-i18next';

class AuthorizationTab extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <h4>{t('authorizationLogsTitle')}</h4>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(AuthorizationTab);