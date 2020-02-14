import React from 'react';
import { Services } from '../Services/Services'
import { Container } from 'reactstrap';

export class Home extends React.Component {
    render() {
        return (
            <Container>
                <Services />
            </Container>            
        );
    }
}
