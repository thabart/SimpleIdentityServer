import React, { Component } from "react";
import { translate } from 'react-i18next';

class Manage extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <h4>{t('manageTitle')}</h4>
            <div className="row">
                <div className="col-md-6">
                    <button className="btn btn-default">{t('exportSettings')}</button>
                </div>
                <div className="col-md-6">
                    <form className="form-group">
                        <label>{t('importSettings')}</label>
                        <input type="file" />
                        <button className="btn btn-default">{t('upload')}</button>
                    </form>
                </div>
            </div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(Manage);