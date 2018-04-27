import React, { Component } from "react";
import { translate } from 'react-i18next';

class ViewClient extends Component {
    constructor(props) {
        super(props);
        this.refreshData = this.refreshData.bind(this);
    }

    /**
    * Refresh the client information.
    */
    refreshData() {

    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('clientTitle')}</h4>
                <i>{t('clientShortDescription')}</i>
            </div>
            <div className="card">
                <div className="body">
                    {t('clientContent')}
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
        this.refreshData();
    }
}

export default translate('common', { wait: process && !process.release })(ViewClient);