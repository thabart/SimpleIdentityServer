import React, { Component } from "react";
import { translate } from 'react-i18next';

class TokenTab extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <h4>{t('tokenSettingsTitle')}</h4>
            <form>
                <div className="form-group">
                    <label>{t('accessTokenExpirationTime')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('authorizationCodeExpirationTime')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('identityTokenExpirationTime')}</label>
                    <input type="text" className="form-control" />
                </div>
                <div className="form-group">
                    <label>{t('refreshTokenExpirationTime')}</label>
                    <input type="text" className="form-control" />
                </div>
                <button className="btn btn-default">{t('saveChanges')}</button>
            </form>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(TokenTab);