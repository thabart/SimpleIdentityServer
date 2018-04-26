import React, { Component } from "react";
import { translate } from 'react-i18next';

class About extends Component {
    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('aboutTitle')}</h4>
                <i>{t('aboutShortDescription')}</i>
            </div>
            <div className="card">
                <div className="body">
                    {t('aboutContent')}
                </div>
            </div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(About);