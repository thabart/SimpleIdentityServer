import React, { Component } from "react";
import { translate } from 'react-i18next';

class Manage extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('manageTitle')}</h4>
                <i>{t('manageShortDescription')}</i>
            </div>
            <div className="container-fluid">
                
            </div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(Manage);