import React, { Component } from "react";
import { translate } from 'react-i18next';

class JsonWebSignatureTab extends Component {
    render() {
        return (<div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(JsonWebSignatureTab);