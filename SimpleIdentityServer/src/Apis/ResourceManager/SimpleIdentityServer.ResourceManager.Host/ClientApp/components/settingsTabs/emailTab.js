import React, { Component } from "react";
import { translate } from 'react-i18next';

class EmailTab extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <h4>{t('emailSettingsTitle')}</h4>
            <form>
                <div className="form-group">
                    <label>{t('emailFromName')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('emailFromAddress')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('emailSubject')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('emailBody')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('emailSmtpHost')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('emailSmtpPort')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('emailUseSsl')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('emailUsername')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('emailPassword')}</label>
                    <input type="text" className="form-control" />
                </div>
                <button className="btn btn-default">{t('saveChanges')}</button>
            </form>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(EmailTab);