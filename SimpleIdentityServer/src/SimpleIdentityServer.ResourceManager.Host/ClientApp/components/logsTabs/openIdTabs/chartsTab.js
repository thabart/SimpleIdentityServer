import React, { Component } from "react";
import { translate } from 'react-i18next';

class ChartsTab extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (<div>

        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(ChartsTab);