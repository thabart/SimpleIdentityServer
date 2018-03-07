import React, { Component } from "react";
import { translate } from 'react-i18next';

class TwilioTab extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <h4>{t('twilioSettingsTitle')}</h4>
            <form>
                <div className="form-group">
                    <label>{t('twilioSidAccount')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('twilioAuthenticationToken')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('twilioFromNumber')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('twilioMessage')}</label>
                    <input type="text" className="form-control" />
                </div>
                <button className="btn btn-default">{t('saveChanges')}</button>
            </form>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(TwilioTab);