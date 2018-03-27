import React, { Component } from "react";
import { translate } from 'react-i18next';

class AuthorizationTab extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="row">
            <div className="col-md-12">
                <div className="card">
                    <div className="header">
                        <h4>{t('authorizationLogsTitle')}</h4>
                    </div>
                    <div className="body">

                    </div>
                </div>
            </div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(AuthorizationTab);