import React from 'react';
import { List as Services } from '../Services/List'
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
