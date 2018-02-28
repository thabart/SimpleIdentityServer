import React, { Component } from "react";
import { withRouter } from 'react-router-dom';
import { translate } from 'react-i18next';

class Resources extends Component {
    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <h4>{t('resourcesTitle')}</h4>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(Resources));